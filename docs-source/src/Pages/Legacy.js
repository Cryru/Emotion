import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { Row, Col } from 'reactstrap';
import style from '../pages.css';
import Highlight from 'react-highlight';

export default class Legacy extends Component {
  constructor(props) {
    super(props)
  }
  render() {
      return  (
        <div>
            <Row className='row-reset'>
                <Col xs='12' className='col-reset pagePaddingHeader'>
                    <div>tetete</div>
                </Col>
            </Row>
            <Row className='row-reset'>
                <Col xs='6' className='col-reset pagePadding'>
                    <p className='pageText'>adadasdasdasdsadadsadasdasdasdasda</p>
                    <Highlight className='cs'>
{`string input = Console.ReadLine();
Console.Clear();

//comment highlight
string[] split = input.Split(' ');

for (int i = 0; i < split.Length; i++)
{
    split += true ? "test" : "falsetest";
}
`}
                    </Highlight>
                </Col>
            </Row>
        </div>
    );
   }
}