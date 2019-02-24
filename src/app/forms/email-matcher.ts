import { ErrorStateMatcher } from "@angular/material";
import { FormGroupDirective, NgForm, FormControl } from "@angular/forms";

export class EmailErrorStateMatcher implements ErrorStateMatcher {
    isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
      const isSubmitted = form && form.submitted;
      return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
    }
  }