import { Component, OnInit } from '@angular/core';
import { ValuesService } from 'src/app/services/values.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  constructor(public valuesService: ValuesService) { }

  ngOnInit() {
    this.getValues();
  }
  getValues() {
    this.valuesService.getValues().subscribe((data) => {
      console.log(data);
    });
  }
}
