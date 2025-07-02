import React, { useState } from 'react';
import SignIn from './Authentication/SignIn';
import ForgotPassword from './Authentication/password';
import './App.css';

function App() {
  const [showForgotPassword, setShowForgotPassword] = useState(false);

  const handleForgotPasswordClick = () => {
    setShowForgotPassword(true);
  };

  return (
    <div className="App">
      {/* Decorative circles shown on all screens */}
      <div className="circle-one"></div>
      <div className="circle-two"></div>
      <div className="circle-three"></div>

      {/* Conditional rendering */}
      {showForgotPassword ? (
        <>
          <div className="ellipse"></div> {/* Only shown with ForgotPassword */}
          <ForgotPassword />
        </>
      ) : (
        <SignIn onForgotPasswordClick={handleForgotPasswordClick} />
      )}
    </div>
  );
}

export default App;
