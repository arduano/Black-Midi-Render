import { Component, OnInit } from '@angular/core';
import { EmailErrorStateMatcher } from '../email-matcher';
import { FormControl, Validators, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-submission-form',
  templateUrl: './submission-form.component.html',
  styleUrls: ['../form-styles.less', './submission-form.component.less']
})
export class SubmissionFormComponent implements OnInit {
  submissionForm = new FormGroup({
    email: new FormControl('', [
      Validators.required,
      Validators.email,
    ]),
    artist: new FormControl(''),
    link: new FormControl(''),
    comment: new FormControl(''),
  });

  matcher = new EmailErrorStateMatcher();
  constructor() { }

  ngOnInit() {
  }

  onSubmit() {
    console.log('Is invalid:', this.submissionForm.invalid);
    console.log(this.submissionForm.value);
  }
}
