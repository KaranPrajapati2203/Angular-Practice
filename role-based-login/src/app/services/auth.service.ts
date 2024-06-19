import { Injectable } from '@angular/core';
import {jwtDecode} from 'jwt-decode'

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  getUser(){
    var token=localStorage.getItem("token");
    if(!token)
    {
      return null;
    }
    var decodeToken:any=jwtDecode(token);
    const userDetail={
      Role:decodeToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    };
    console.log(userDetail);
    return userDetail;
  }
}
