# **Correlation**
When you have a microservices architecture or when your application is divided into multiple components/systems where a single request hops from one to another, its important that you should be able to track each request throughout its lifecycle. To help in such a scenario, correlation Ids can be used to detect and troubleshoot errors creeping in middleware systems.

To define this a little further, lets assume you have a microservices architecture and each request will be served by one or multiple microservices, all working asynchronously on their specific tasks and ultimately coming together to generate the response. In such a scenario, if one of the service fails, it will be hard to determine by looking at the huge pile of logs for the failed request and its root cause further adding up time on troubleshooting and fixing the error. In such a scenario, a correlation id will help identify a failure (or a success with invalid response) for a single request.

The basic idea is to track every request and a way to tie all of these service components. And one way to relate or tie these service components is to have a unique identifier i.e. Correlation Id that will help correlate all of the different micro tasks to the same macro operation.

This part is divided into 2 components, i.e. API and UI. API is built over dotnet core and UI using Angular (v7). 

**Pre requisites**
* Basic understanding of dotnet core application
* Basic understanding of Angular
* Basic understanding of logging

**Technology Stack**
* dotnet core (v2.2)
* Angular (v7)
* Azure App Insights

## **Implementing correlation Id's in Web API**
The dotnet core Web API included has the default template created using 

```
dotnet core new
```

or by using Visual Studio's built in templates (Go to File > New > Project > .NET Core > ASP.NET Core Web Application > API)

The idea here is to not create a unique correlation id, as the API will be consumed by another application but to reuse a provided correlation id. The best place to fetch this unique identifier is from the request headers. As this is a custom header, we will have to expose this to get away with the CORS issue of blocked headers for the consumers of our API. This can be done in Startup class. 

```C#
services.AddCors(options => {
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("x-correlation-id");
        });
});
```

The above code will expose the headers that we want i.e. "x-correlation-id" (this can be any name/key).

Although, we are supporting to accept this header, we still need to verify if the correlation id header is present in a request and if not, we add it. This is helpful, when the API is consumed by third party and we still need to correlate any request in our system. To do so, we will use the built in owin middleware (no need to create a custom one as nothing fancy here) inside the Startup class > Configure(IApplicatinoBuilder app, IHostingEnvironment env) method.

```C#
app.Use(async (context, next) =>
{
    // add a correlationId if doesn't exist which can be reused in next handle
    if (!context.Request.Headers.ContainsKey("x-correlation-id"))
        context.Request.Headers.Add("x-correlation-id", System.Guid.NewGuid().ToString());
    context.Response.Headers.Add("x-correlation-id", context.Request.Headers["x-correlation-id"]);
    await next();
});
```

The next step is to include logging of all the events and exceptions in our API. We are using Microsoft.ApplicationInsights.AspNetCore (v2.6.1) NuGet package. In the Startup class we register the App Insights telemetry

```C#
services.AddApplicationInsightsTelemetry(Configuration);
```

And we have created a logging service, where we use TelemetryClient from the above installed NuGet package to log events, exceptions, traces, dependencies, etc. This service is AiLogger that implements ILogger. In this service, you will notice some dependency over IHttpContextAccessor and ApplicationSettings. The IHttpContextAccessor is helpful in getting the request from the current HttpContext to fetch the correlation id provided. The call to fetch it is pretty simple 

```C#
httpContextAccessor.HttpContext.Request.Headers["x-correlation-id"]
```

The methods/functions in this logger service will then fetch the correlation id whenever required using the IHttpContextAccessor. We created a getter propety in the logger service to fetch correlation id from request header and using the property in any of the methods/functions.

```C#
public void LogEvent(string eventName)
{
    var customProperties = new Dictionary<string, string> {
        { "CorrelationId", CorrelationId }
    };
    telemetryClient.TrackEvent(eventName, customProperties);
}

public void LogException(Exception ex)
{
    var customProperties = new Dictionary<string, string> {
        { "CorrelationId", CorrelationId }
    };
    telemetryClient.TrackException(ex, customProperties);
}
```

Once we have the logger in place, its simple to inject this logger in any of your calling code, controller, etc. and start logging events, exceptions, etc.

## Implementing correlation Id's in Angular
The concept will remain same for any other application. 

This is our entry application, from where we will call the API. Lets start by creating a new angular application 

```
ng new UI
```

Once the angular application is created using the default template, we will start by implementing the correlation id while calling the API. In this application we will create one dashboard component, one service that will call an one of the endpoint. Once the component and service is in place, we have to do one thing while calling the API, that is to append the provided correlation id header with each request. Note that this is a consumer for the API and is responsible for passing a new correlation Id for each request when making a request to API. A simplest way of doing this is to write an HttpInterceptor (called interceptor from now on) available from Angular v4. 

To define interceptors in short, its one common place where you can modify, handle any request and its response. You can modify headers, add retry logics, delays, handle errors in one common place, instead of repeating it in every service, component.

To start with, we create an interceptor like so

```TypeScript
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { retry, delay, map, catchError } from 'rxjs/operators';
import { LoggingService } from '../services/logging.service';

@Injectable()
export class RequestInterceptor implements HttpInterceptor {
    properties: { [name: string]: string } = {};

    constructor(public loggingService: LoggingService) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        request = request.clone({
            setHeaders: {
                'x-correlation-id': this.createUniqueId() // append a unique correlation id with you request
            }
        });
        return next.handle(request)
            .pipe(
                retry(2),
                delay(3000),
                map((event: HttpEvent<any>) => {
                    if(event instanceof HttpResponse) {
                        this.properties['CorrelationId'] = event.headers.get('x-correlation-id');
                        this.loggingService.logEvent(event.url, this.properties); // log success event to app insights
                    }
                    return event;
                }),
                catchError((error: HttpErrorResponse) => {
                    // attach required properties in case of exception for troubleshooting
                    this.properties['name'] = error.name;
                    this.properties['status'] = `${error.status} - ${error.statusText}`;
                    this.properties['message'] = error.message;
                    this.properties['url'] = error.url;
                    this.properties['CorrelationId'] = error.headers.get('x-correlation-id');
                    this.loggingService.logException(error, null, this.properties); // log exception to app insights
                    // write some logic or service to show an error dialog
                    return throwError(error);
                }));
    }

    // create unique guid for correlation
    public createUniqueId() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            let r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}
```

Notice that this class implements HttpInterceptor that has intercept method that is used to intercept a request. Interceptors are built for the purpose of intercepting every single request that goes out of your application. Once request is captured here, we can clone it and add the required headers, body, params, etc. For now, our only need is to add correlation Id for every request. Once that is done, we just pass the request to the next handle, that can be another interceptor and if not, it will make an http call. Once the request is executed, based on the response, if its error, we have a retry logic with a delay of 3 seconds. After that, for a successfull response, it goes in to your HttpResponse where we can use the event object to capture the correlation id from header and use it to log an event; and for a failed response, it goes to catchError where you capture few props from error object along with the correlation id and log an exception. There is a logging service which is used/injected that has similar methods like API for logging events, exceptions, in to Application Insights. We have used applicationinsights-js npm package for Angular application.

Once the interceptor is in place, it requires to be registered. This can be done in your base module (app.module) in the providers section.

```TypeScript
providers: [
    {
        provide: HTTP_INTERCEPTORS,
        useClass: RequestInterceptor,
        multi: true
    }
]
```

This is it. You have the correlation id in place for your angular application and have successfully implemented tracking of a request from your UI to API and back. This can be further extended if you have another component and by using the same logic of passing the correlation id from request to response. 

***

#### Further reading
* [dotnet core Enabling CORS](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.2)
* [ dotnet core Owin middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/owin?view=aspnetcore-2.2)
* [Getting Started with Application Insights for ASP.NET Core](https://github.com/Microsoft/ApplicationInsights-aspnetcore/wiki/Getting-Started-with-Application-Insights-for-ASP.NET-Core)
* [Angular HttpInterceptor](https://angular.io/api/common/http/HttpInterceptor)
* [Application Insights NPM (Angular)](https://www.npmjs.com/package/applicationinsights-js)
