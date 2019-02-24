import { routerNgProbeToken } from '@angular/router/src/router_module';
import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { formTransition } from 'src/app/router.animations';

@Component({
  selector: 'app-contact-page',
  animations: [formTransition],
  templateUrl: './contact-page.component.html',
  styleUrls: ['../page-styles.less', './contact-page.component.less']
})
export class ContactPageComponent implements OnInit {

  submitted: boolean;
  constructor(private route: ActivatedRoute, public router: Router) { }
  
  transition(o){
    return o.isActivated ? o.activatedRoute : '';
  }

  ngOnInit() {
  }
}
