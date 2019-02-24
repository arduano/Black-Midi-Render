import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class GoogleFormsService {

  constructor(private http: HttpClient) {
  }

  submitForm(url: string, data: any) {
    url = 'https://cors-anywhere.herokuapp.com/' + url;
    let fdata = new FormData()
    console.log("sending submission");

    for (let key in data) {
      fdata.append(key, data[key]);
    }
    return this.http.post(url, fdata).toPromise().catch(
      e => {
        return e.status == 200;
      }
    )
  }
}
