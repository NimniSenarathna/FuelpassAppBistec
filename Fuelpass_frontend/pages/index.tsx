import React from 'react';
import Router from 'next/router';
import '../styles/Homepage.css';

const MainHomePage = () => {
  const handleRegisterClick = () => {
    Router.push('/Registerpage');
  };

  const handleIssueFuelClick = () => {
    Router.push('/Issuefuel');
  };

  return (
    <div className="container">
      <h1 className="title">Fuel Pass Application</h1>
      <div className="buttonContainer">
        <button className="button" onClick={handleRegisterClick}>
          Register Here
        </button>
        <button className="button loginButton">Log in</button>
        <button className="button" onClick={handleIssueFuelClick}>
          Issue Fuel to User
        </button>
      </div>
    </div>
  );
};

export default MainHomePage;
