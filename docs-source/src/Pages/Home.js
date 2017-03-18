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
                                {`SoulEngine is a 2D XNA game engine with focus on making the development process easier while maintaining control over your code.`}
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