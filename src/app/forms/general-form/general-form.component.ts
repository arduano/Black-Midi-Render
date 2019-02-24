import { GoogleFormsService } from './../../services/google-forms/google-forms.service';
import { Component, OnInit, Output } from '@angular/core';
import { EmailErrorStateMatcher } from '../email-matcher';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { generate } from 'rxjs';
import { Router } from '@angular/router';
import { routerNgProbeToken } from '@angular/router/src/router_module';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'app-general-form',
  templateUrl: './general-form.component.html',
  styleUrls: ['../form-styles.less', './general-form.component.less']
})
export class GeneralFormComponent implements OnInit {

  @Output() submitted: EventEmitter<void> = new EventEmitter();

  generalForm = new FormGroup({
    email: new FormControl('', [
      Validators.required,
      Validators.email,
    ]),
    subject: new FormControl(''),
    message: new FormControl(''),
  });
  emailFormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);

  submitting: boolean = false;
  submissionError: boolean = false;

  matcher = new EmailErrorStateMatcher();

  constructor(public gforms: GoogleFormsService, private router: Router) { }

  ngOnInit() {
  }

  async onSubmit() {
    console.log('Is invalid:', this.generalForm.invalid);
    console.log(this.generalForm.value);
    if(!this.generalForm.invalid){
      let fdata = {
        'entry.16121598': this.generalForm.value.email,
        'entry.1135265470': this.generalForm.value.subject,
        'entry.1638366982': this.generalForm.value.message,
      }
      let url = 'https://docs.google.com/forms/d/e/1FAIpQLSfe13SpJKX73C7yDpb58G5Gqnt9eqvmbRUsTcus6ITPhAYedg/formResponse'
      this.submitting = true
      this.submissionError = false;
      this.gforms.submitForm(url, fdata).then(success => {
        this.submitting = false;
        if(success){
          //this.router.navigate(['/contact/submitted'])
          this.submitted.emit()
        }
        else{
          this.submissionError = true;
        }
      });
    }
  }
}
