import { TestBed } from '@angular/core/testing';

import { GoogleFormsService } from './google-forms.service';

describe('GoogleFormsService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: GoogleFormsService = TestBed.get(GoogleFormsService);
    expect(service).toBeTruthy();
  });
});
