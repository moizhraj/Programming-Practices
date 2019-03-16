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