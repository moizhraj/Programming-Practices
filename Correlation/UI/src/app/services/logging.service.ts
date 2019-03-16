import { Injectable } from '@angular/core';
import { AppInsights } from 'applicationinsights-js';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LoggingService {

  constructor() { 
    if(!AppInsights.config) {
      AppInsights.downloadAndSetup(environment.appInsights);
    }
  }

  logEvent(name: string, properties?:  { [name: string]: string }, measurements?:  { [name: string]: number }) {
    AppInsights.trackEvent(name, properties, measurements);
  }
  logException(exception: Error, handledAt?: string, properties?:  { [name: string]: string }, measurements?:  { [name: string]: number }) {
    AppInsights.trackException(exception, handledAt, properties, measurements);
  }
}
