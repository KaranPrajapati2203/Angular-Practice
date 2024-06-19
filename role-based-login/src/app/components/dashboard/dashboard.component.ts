import { Component, inject } from '@angular/core';
import { LoginComponent } from '../login/login.component';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [LoginComponent,CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  Authservice=inject(AuthService)
  userDetail:any;

  ngOnInit()
  {
    this.userDetail=this.Authservice.getUser();
  }


}
