import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { EmailErrorStateMatcher } from '../email-matcher';

@Component({
  selector: 'app-takedown-form',
  templateUrl: './takedown-form.component.html',
  styleUrls: ['../form-styles.less', './takedown-form.component.less']
})
export class TakedownFormComponent implements OnInit {
  takedownForm = new FormGroup({
    email: new FormControl('', [
      Validators.required,
      Validators.email,
    ]),
    name: new FormControl(''),
    url: new FormControl(''),
    song: new FormControl(''),
    artwork: new FormControl(''),
    other: new FormControl(''),
    something: new FormControl(''),
    comment: new FormControl(''),
    submit: new FormControl(''),
  });

  matcher = new EmailErrorStateMatcher();
  constructor() { }

  ngOnInit() {
  }

  onSubmit() {
    console.log('Is invalid:', this.takedownForm.invalid);
    console.log(this.takedownForm.value);
  }}
