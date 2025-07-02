import React, { useState } from 'react';
import './sign_in.css';
import MailIcon from '../assets/icons/mail2.svg';
import KeyIcon from '../assets/icons/key2.svg';
import VisibilityOffIcon from '../assets/icons/visibility_off.svg';
import ImageGenForWeb from '../assets/images/iMAGE gEN FOR WEB.svg';
import './Sign_in.js';





const SignIn = ({ onForgotPasswordClick }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const handleLogin = async () => {
    try {
      const response = await fetch('http://localhost:5037/api/user/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        const message = await response.text();
        console.log('Login successful:', message);
        setError('');
        // TODO: Redirect or store auth token
      } else {
        const errorText = await response.text();
        setError(errorText || 'Login failed');
      }
    } catch (err) {
      console.error('Login error:', err);
      setError('Network error');
    }
  };

  return (
    <div className="signin-container">
      <div className="logo-container">
        <span className="logo-bold">singular</span>
        <span className="logo-light">express</span>
      </div>

      <div className="auth-content">
        <div className="column left-column">
          <div className="left-inner-column" style={{ marginLeft: '50px' }}> {/* Adjusted margin-left */}
            <div className="adjusted-content">
              <div className="welcome-text">Welcome!</div>
              <div className="log-details">
                <div className="input-group">
                  <img src={MailIcon} alt="email icon" className="input-icon-mail" />
                    <input
                      type="email"
                      placeholder="Enter your email"
                      className="input-field"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                    />
                </div>
                <div className="input-group">
                  <img src={KeyIcon} alt="password icon" className="input-icon-key" />
                    <input
                      type={showPassword ? 'text' : 'password'}
                      placeholder="Enter your password"
                      className="input-field"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                    />
                  <img
                    src={VisibilityOffIcon}
                    alt="toggle visibility"
                    className="visibility-icon"
                    onClick={togglePasswordVisibility}
                    style={{ cursor: 'pointer' }}
                  />
                </div>
                {error && <div style={{ color: 'red', marginBottom: '10px' }}>{error}</div>}
                <button className="sign-in-button" onClick={handleLogin}>
                  Sign in
                </button>
                <div
                  className="forgot-password"
                  onClick={onForgotPasswordClick} 
                  style={{ cursor: 'pointer' }}
                >
                  Forgot password
                </div>
                <div className="footer-text">
                  Privacy Policy | Terms & Conditions
                  <br />
                  Copyright © 2025 Singular Systems. All rights reserved.
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="column right-column">
          <img src={ImageGenForWeb} alt="Image Gen For Web" className="right-column-image" />
        </div>
      </div>
    </div>
  );
};

export default SignIn;