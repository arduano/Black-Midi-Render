import { UIService } from './services/ui/ui.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { StartPageComponent } from './pages/start-page/start-page.component';
import { AboutPageComponent } from './pages/about-page/about-page.component';
import { MainComponent } from './core/main/main.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { InfoBoxComponent } from './misc/info-box/info-box.component';
import { WorksPageComponent } from './pages/works-page/works-page.component';
import { SetupPageComponent } from './pages/setup-page/setup-page.component';
import { ContactPageComponent } from './pages/contact-page/contact-page.component';
import { PageScrollerComponent } from './core/page-scroller/page-scroller.component';
import { SocialMediaComponent } from './misc/social-media/social-media.component';
import { MatButtonModule, MatCheckboxModule, MatCardModule, MatFormFieldModule, MatInputModule, MatNativeDateModule, MatRadioModule, MatProgressBarModule,} from '@angular/material';
import { GeneralFormComponent } from './forms/general-form/general-form.component';
import { SubmissionFormComponent } from './forms/submission-form/submission-form.component';
import { TakedownFormComponent } from './forms/takedown-form/takedown-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GoogleFormsService } from './services/google-forms/google-forms.service';
import { HttpClient } from 'selenium-webdriver/http';
import { HttpClientModule } from '@angular/common/http';
import { FormSuccessComponent } from './forms/form-success/form-success.component';


@NgModule({
  declarations: [
    AppComponent,
    StartPageComponent,
    AboutPageComponent,
    MainComponent,
    InfoBoxComponent,
    WorksPageComponent,
    SetupPageComponent,
    ContactPageComponent,
    PageScrollerComponent,
    SocialMediaComponent,
    GeneralFormComponent,
    SubmissionFormComponent,
    TakedownFormComponent,
    FormSuccessComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule, 

    MatButtonModule, MatCheckboxModule, MatCardModule, 
    MatFormFieldModule, MatInputModule, MatRadioModule,
    MatProgressBarModule,

    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    MatNativeDateModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
