import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-works-page',
  templateUrl: './works-page.component.html',
  styleUrls: ['../page-styles.less', './works-page.component.less']
})
export class WorksPageComponent implements OnInit {

  url: string = window.location.pathname;

  constructor(private route: ActivatedRoute, public router: Router) { }

  ngOnInit() {
    this.router.events.subscribe(e => {
      this.url = window.location.pathname
    })
  }

}

