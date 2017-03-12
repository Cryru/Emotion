import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import { Row, Col, Container } from 'reactstrap';
import style from '../pages.css';
import Highlight from 'react-highlight';

export default class Legacy extends Component {
    constructor(props) {
        super(props)
    }
    render() {
        return (
            <Container>
                <Row className='row-reset'>
                    <Col xs="12" className='col-reset'>
                        <div className='pageHeader-h1'>SoulEngine 2017 Documentation</div>
                    </Col>
                </Row>
                    <Row className='row-reset'>
                        <Col xs="12" className='col-reset'>
                            <p className='pageText'>
                                {`SoulEngine is a 2D XNA/Monogame based game engine created on the idea of hands-on code writing and in-depth control
                                as opposed to the script based approach big engines have. In addition it aims to make the development process easier, 
                                and to shorten the time it takes between coming up with an idea, getting a prototype ready, and even releasing a product.`}
                            </p>
                        </Col>
                    </Row>
                    <Row className='row-reset'>
                         <Col xs="12" className='col-reset'>
                            <p className='pageText'>
                                {`Useful Links: `}
                            </p>
                            <ul>
                                <li><a href="https://github.com/Cryru/SoulEngine">Official GitHub Repository</a></li>
                                <li><a href="https://github.com/Cryru/SoulEngine/issues">Help Fix Bugs</a></li>
                            </ul>
                        </Col>
                    </Row>
            </Container>
        );
    }
}