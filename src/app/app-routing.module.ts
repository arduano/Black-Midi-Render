import { FormSuccessComponent } from './forms/form-success/form-success.component';
import { TakedownFormComponent } from './forms/takedown-form/takedown-form.component';
import { AboutPageComponent } from './pages/about-page/about-page.component';
import { MainComponent } from './core/main/main.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule, ActivatedRouteSnapshot, DetachedRouteHandle, RouteReuseStrategy } from '@angular/router';
import { StartPageComponent } from './pages/start-page/start-page.component';
import { WorksPageComponent } from './pages/works-page/works-page.component';
import { ContactPageComponent } from './pages/contact-page/contact-page.component';
import { SetupPageComponent } from './pages/setup-page/setup-page.component';
import { GeneralFormComponent } from './forms/general-form/general-form.component';
import { SubmissionFormComponent } from './forms/submission-form/submission-form.component';


const routes: Routes = [
  { path: '', redirectTo: 'start', pathMatch: 'full' },
  {
    path: '', component: MainComponent, children: [
      { path: 'start', component: StartPageComponent },
      { path: 'about', component: AboutPageComponent },
      { path: 'works', component: WorksPageComponent },
      { path: 'setup', component: SetupPageComponent },
      { path: 'contact', component: ContactPageComponent},
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }


