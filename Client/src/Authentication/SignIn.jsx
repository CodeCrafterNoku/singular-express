import React from 'react';
import './sign_in.css';
import signinImage from '../assets/signin_image.png'; // Placeholder for the image

const SignIn = () => {
  return (
    <div className="signin-container">
      {/* SingularExpress Logo (Top Left) */}
      <div className="logo-container">
        <span className="logo-bold">singular</span>
        <span className="logo-light">express</span>
      </div>

      {/* Two-column sign-in content */}
      <div className="auth-content">
        <div className="column left-column">
          {/* Left column content with "Welcome!" text */}
          {/* <div className="welcome-text">Welcome!</div> */}
        </div>
        <div className="column right-column">
          {/* Right column for the image (placeholder for now) */}
          {/* <img src={signinImage} alt="Sign In" className="signin-image_Photoroom" /> */}
        </div>
      </div>
    </div>
  );
};

export default SignIn;