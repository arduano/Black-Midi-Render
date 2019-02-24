import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TakedownFormComponent } from './takedown-form.component';

describe('TakedownFormComponent', () => {
  let component: TakedownFormComponent;
  let fixture: ComponentFixture<TakedownFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TakedownFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TakedownFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
