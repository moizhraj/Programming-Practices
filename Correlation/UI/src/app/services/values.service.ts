import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ValuesService {

  constructor(public httpClient: HttpClient) { }

  getValues(): Observable<any> {
    return this.httpClient.get<any>(environment.apiBasePath + '/api/v1.0/values/getall');
  }
}
