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
                    <Col xs="12" className='col-reset Header'>
                        <div className='pageHeader-h1'>Legacy Support for SoulEngine 2016</div>
                    </Col>
                </Row>
                    <Row className='row-reset'>
                        <Col xs="12" className='col-reset'>
                            <p className='pageText'>
                                {`To allow old projects to be ported over a compatibility layer for older SoulEngine versions
                        is located in the`} <a className='filePath' href="https://github.com/Cryru/SoulEngine/tree/master/src/Legacy">src/Engine/Legacy</a> {`folder, and
                        can be accessed through the SoulEngine.Legacy namespace.`}
                            </p>
                            <p className='pageText'>
                                {`The isn't direct copy-pasted from the older version, rather it is a syntax converter which makes use of new features behind the scenes.
                        The reason for this is to prevent duplicate code and polution such as having two separate settings for window width for example.
                        As such it is not guaranteed that both will behave the same in all situations.`}
                            </p>
                            <p className='pageText'>
                                {`Version 1 Dev(3/10/2017) fully implements the legacy Core and Settings.`}
                            </p>
                        </Col>
                    </Row>
                    <div className='spacer-small' />
                    <Row className='row-reset'>
                        <Col xs="5" className='col-reset'>
                            <Highlight className='cs'>
                                {
                                    `public ObjectBase(Texture Image = null)
{

    if (Image == null)
    {
        Image = new Texture();
    }



    this.Image = Image;
}`
                                }
                            </Highlight>
                            <span className='t-center'>Original Code</span>
                        </Col>
                        <Col xs="1" className='col-reset' />
                        <Col xs="5" className='col-reset'>
                            <Highlight className='cs'>
                                {
                                    `public ObjectBase(Texture Image = null)
{
    actualObject = 
    GameObject.GenericDrawObject;

    if (Image == null)
    {
        actualObject
            .Component<ActiveTexture>()
            .Texture = Image.Image;
    }
}`
                                }
                            </Highlight>
                            <span className='t-center'>Adapted Code</span>
                        </Col>
                    </Row>
                    <div className='spacer' />
                    <Row className='row-reset'>
                        <Col xs="12" className='col-reset'>
                            <div className='pageHeader-h2'>How To Port Over</div>
                             <p className='pageText'>
                                {`TODO`}
                            </p>
                        </Col>
                    </Row>
                    <div className='spacer' />
                    <Row className='row-reset'>
                        <Col xs="6" className='col-reset'>
                            <p className='pageText'>
                                {`Namespaces: `}
                            </p>
                            <ul>
                                <li>SoulEngine.Legacy</li>
                                <li>SoulEngine.Legacy.Objects</li>
                            </ul>
                        </Col>
                         <Col xs="6" className='col-reset'>
                            <p className='pageText'>
                                {`Useful Links: `}
                            </p>
                            <ul>
                                <li><a href="https://github.com/Cryru/SoulEngine-2016">SoulEngine 2016</a></li>
                            </ul>
                        </Col>
                    </Row>
            </Container>
        );
    }
}