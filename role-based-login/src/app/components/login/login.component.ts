import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';  // Import ReactiveFormsModule
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(private fb: FormBuilder, private router: Router, private http: HttpClient) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void { }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const loginData = this.loginForm.value;

      // Make the API call
      this.http.post('https://localhost:7227/api/Users/login', loginData).subscribe(
        (response: any) => {
          console.log('Login successful', response);
          localStorage.setItem("token",response.token);
          this.router.navigateByUrl('/dashboard'); // Navigate to the dashboard
        },
        (error: any) => {
          console.error('Login failed', error);
        }
      );
    } else {
      console.log('Form not valid');
    }
  }
  
}
