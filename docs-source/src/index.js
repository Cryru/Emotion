import React from 'react';
import ReactDOM from 'react-dom';
import Main from './Main';
import { Router, Route, Link, browserHistory } from 'react-router'
import 'bootstrap/dist/css/bootstrap.css';

ReactDOM.render(
  <Router history={browserHistory}>
    <Route path="/" component={Main}>
      <Route path="page" component={Main}>
        <Route path="/page/:pageId" component={Main}/>
      </Route>
      <Route path="*" component={Main}/>
    </Route>
  </Router>,
  document.getElementById('root')
);
