import React, { Component } from 'react';
import { Row, Col } from 'reactstrap';
import style from './Main.css';
import { Sidebar, SidebarItem } from 'react-responsive-sidebar';
import Legacy from './Pages/Legacy';
import { Link } from 'react-router'

export default class Main extends Component {
  constructor(props) {
    super(props)
  }
  render() {

    const navigationBar = [
      <div className='navHeaderBox'><Link to="/"><div className='navHeaderLink'>SoulEngine Documentation</div></Link></div>,
      <SidebarItem href='/page/legacy'><div className='navItem'>Legacy Support</div></SidebarItem>
    ]
    let page;

    console.dir(this.props.params.pageId);

    switch(this.props.params.pageId)
    {
      case 'legacy':
       page = <Legacy />
      break;
      case undefined:
       page = <div>WELCOME! SELECT A PAGE FROM THE NAVIGATION :D</div>
      break;
      default:
       page = <div>PAGE NOT FOUND</div>
      break;
    }

    return (
      <div className='navReset'>
        <Sidebar content={navigationBar}
          background='#6324d8'
          backdrop={false}
        >
          {page}
        </Sidebar>
      </div>
    );
  }
}
