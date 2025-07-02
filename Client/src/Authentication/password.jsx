import React, { useState } from 'react';
import './password.css';
import MailIcon from '../assets/icons/mail2.svg';
import KeyIcon from '../assets/icons/key2.svg';
import VisibilityOffIcon from '../assets/icons/visibility_off.svg';

const ForgotPassword = () => {
  const [isOtpSent, setIsOtpSent] = useState(false); // State to track if OTP has been sent
  const [showPassword, setShowPassword] = useState(false); // State to toggle password visibility
  const [currentStep, setCurrentStep] = useState('request'); // Track the current step

  const handleRequestResetLink = () => {
    setIsOtpSent(true); // Change state to indicate OTP has been sent
  };

  const handleSubmit = () => {
    setCurrentStep('newPassword'); // Change to new password step
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword); // Toggle password visibility
  };

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
          {/* Show the password reset image on the left */}
          <img
            src={require('../assets/images/password_image.png')}
            alt="Reset Password"
            className="password-image"
          />
        </div>

        <div className="column right-column">
          {/* Conditional Rendering Based on State */}
          {currentStep === 'newPassword' ? (
            <>
              {/* New Password View */}
              <div className="new-password-title">New password</div>
              <div className="new-password-instruction">
                Enter the email address and we will send you instructions to reset your password
              </div>
              <div className="password-input-group">
                <img
                  src={KeyIcon}
                  alt="password icon"
                  className="input-icon-key"
                />
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Enter your new password"
                  className="input-field"
                />
                <img
                  src={VisibilityOffIcon}
                  alt="toggle visibility"
                  className="visibility-icon"
                  onClick={togglePasswordVisibility}
                  style={{ cursor: 'pointer' }}
                />
              </div>
              <div className="password-input-group">
                <img
                  src={KeyIcon}
                  alt="password icon"
                  className="input-icon-key"
                />
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Confirm your new password"
                  className="input-field"
                />
                <img
                  src={VisibilityOffIcon}
                  alt="toggle visibility"
                  className="visibility-icon"
                  onClick={togglePasswordVisibility}
                  style={{ cursor: 'pointer' }}
                />
              </div>
              <button className="request-button" onClick={handleSubmit}>Save</button>
            </>
          ) : isOtpSent ? (
            <>
              {/* OTP View */}
              <div className="otp-title">Check your mail</div>
              <div className="reset-instruction">
                We've sent a verification code to your email address. Please enter the code below to continue resetting your password.
              </div>
              <div className="otp-container">
                {[...Array(4)].map((_, index) => (
                  <input
                    key={index}
                    type="text"
                    className="otp-input"
                    maxLength={1}
                    placeholder=""
                  />
                ))}
              </div>
              <button className="request-button" onClick={handleSubmit}>Submit</button>
            </>
          ) : (
            <>
              {/* Initial View */}
              <div className="forgot-password-title">Forgot your password</div>
              <div className="reset-instruction">
                Enter the email address and we will send you instructions to reset your password
              </div>
              <div className="log-details">
                <div className="input-group">
                  <img src={MailIcon} alt="email icon" className="input-icon-mail" />
                  <input
                    type="email"
                    placeholder="Enter your email"
                    className="input-field"
                  />
                </div>
                <button className="request-button" onClick={handleRequestResetLink}>
                  Request reset link
                </button>
              </div>
            </>
          )}
          {/* Conditionally render the Back to log in link */}
          {currentStep !== 'newPassword' && (
            <div className="back-to-login">Back to log in</div>
          )}
          <div className="footer-text">
            Privacy Policy   |   Terms & Conditions
            <br />
            Copyright © 2025 Singular Systems. All rights reserved.
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;