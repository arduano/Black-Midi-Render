import { AboutPageComponent } from './pages/about-page/about-page.component';
import { MainComponent } from './core/main/main.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule, ActivatedRouteSnapshot, DetachedRouteHandle, RouteReuseStrategy } from '@angular/router';
import { StartPageComponent } from './pages/start-page/start-page.component';
import { WorksPageComponent } from './pages/works-page/works-page.component';
import { SetupPageComponent } from './pages/setup-page/setup-page.component';
import { BenchmarksPageComponent } from './pages/benchmarks-page/benchmarks-page.component';


const routes: Routes = [
  { path: 'Black-Midi-Render', redirectTo: 'Black-Midi-Render/start', pathMatch: 'full' },
  { path: '', redirectTo: 'Black-Midi-Render/start', pathMatch: 'full' },
  {
    path: 'Black-Midi-Render', component: MainComponent, children: [
      { path: 'start', component: StartPageComponent },
      { path: 'about', component: AboutPageComponent },
      { path: 'works', component: WorksPageComponent },
      { path: 'setup', component: SetupPageComponent },
      { path: 'benchmarks', component: BenchmarksPageComponent},
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }


