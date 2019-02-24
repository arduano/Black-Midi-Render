import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-info-box',
  templateUrl: './info-box.component.html',
  styleUrls: ['./info-box.component.less']
})
export class InfoBoxComponent implements OnInit {

  @Input() text: string = 'dolor sit amet, vel ceteros nostrum ut, no erat verterem pro, quo ne epicurei aliquando definitiones. Eos laoreet imperdiet ea. Pro ei quot lorem. Vel dico atomorum cu. Per ei prima equidem habemus.'
  @Input() title: string = "Lorem ipsum"
  @Input() type: string;

  constructor() { }

  ngOnInit() {
  }

}
