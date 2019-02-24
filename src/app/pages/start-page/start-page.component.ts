import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-start-page',
  templateUrl: './start-page.component.html',
  styleUrls: ['../page-styles.less', './start-page.component.less']
})
export class StartPageComponent implements OnInit {

  constructor(public router: Router) { }

  ngOnInit() {
  }

}
