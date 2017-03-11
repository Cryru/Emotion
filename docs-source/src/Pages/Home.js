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
        return (
            <div>
                <Row className='row-reset'>
                    <Col xs="10" className='col-reset'>
                        <div className='pageHeader-h1'>SoulEngine 2017 Documentation</div>
                    </Col>
                </Row>
                    <Row className='row-reset'>
                        <Col xs="10" className='col-reset'>
                            <p className='pageText'>
                                {`SoulEngine is a 2D XNA/Monogame based game engine which provides another layer of 
                                abstraction while maintaining the hands-on code writing associated with XNA and Monogame 
                                as opposed to the script based approach big engines have. The intent is to make game development 
                                easier without losing control. The point is NOT to squeeze performance, but rather to shorten the 
                                time it takes between coming up with an idea, getting a prototype ready, and even releasing a product.`}
                            </p>
                        </Col>
                    </Row>
                    <Row className='row-reset'>
                        <Col xs="10" className='col-reset'>
                            <p className='pageText'>
                                {`To learn more about the major features of the engine browse through the navigation bar.`}
                            </p>
                        </Col>
                    </Row>
                    <Row className='row-reset'>
                         <Col xs="6" className='col-reset'>
                            <p className='pageText'>
                                {`Useful Links: `}
                            </p>
                            <ul>
                                <li><a href="https://github.com/Cryru/SoulEngine">Official GitHub Repository</a></li>
                                <li><a href="https://github.com/Cryru/SoulEngine/issues">Help Fix Bugs</a></li>
                            </ul>
                        </Col>
                    </Row>
            </div>
        );
    }
}