import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

// OOB
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientModule } from '@angular/common/http';
// Custom
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RequestInterceptor } from './helpers/request.interceptor';
import { LoggingService } from './services/logging.service';
import { ValuesService } from './services/values.service';
import { DashboardComponent } from './views/dashboard/dashboard.component';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: RequestInterceptor,
      multi: true
    },
    LoggingService,
    ValuesService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
