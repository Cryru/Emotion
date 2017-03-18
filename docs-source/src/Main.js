import React, { Component } from 'react';
import { Row, Col } from 'reactstrap';
import style from './Main.css';
import { Sidebar, SidebarItem } from 'react-responsive-sidebar';
import { Link } from 'react-router';
import logo from './Icon.ico';

import Legacy from './Pages/Legacy';
import Home from './Pages/Home';

export default class Main extends Component {
  constructor(props) {
    super(props)
  }
  render() {

    let page;
    let navBoxClass = "navHeaderBox";

    switch (this.props.params.pageId) {
      case 'legacy':
        page = <Legacy />
        break;
      case undefined:
        page = <Home />
        navBoxClass = "navHeaderBox navHeaderBoxAtPage";
        break;
      default:
        page = <div>PAGE NOT FOUND</div>
        break;
    }

    const navigationBar = [
      <div className={navBoxClass}><Link to="/SoulEngine"><div className='navHeaderLink'><img className='logoImage' src={logo} /> SoulEngine Documentation</div></Link></div>,
      <SidebarItem href='/SoulEngine/page/object'><div className='navItem'>Object-Component System</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/textures'><div className='navItem'>Animated Textures</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/assets'><div className='navItem'>Assets</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/scene'><div className='navItem'>Scene System</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/event'><div className='navItem'>Event System</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/tiled'><div className='navItem'>Tiled</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/timing'><div className='navItem'>FPS and Timing</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/tamper'><div className='navItem'>Asset Tampering Protection</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/camera'><div className='navItem'>Camera System</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/resolution'><div className='navItem'>Resolution Adaptation</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/jsonfiles'><div className='navItem'>JSON File Management</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/errors'><div className='navItem'>Error Logging</div></SidebarItem>,
      <SidebarItem href='/SoulEngine/page/legacy'><div className='navItem'>Legacy Support</div></SidebarItem>
    ]

    return (
      <div className='navReset'>
        <Sidebar content={navigationBar}
          background='#9b1db7'
          backdrop={false}
        >
          <div className='pagePadding'>
            {page}
          </div>
        </Sidebar>
      </div>
    );
  }
}
