import React from 'react';
import SignIn from './Authentication/SignIn';

function App() {
  return (
    <div className="App">
      {/* Decorative circles */}
      <div className="circle-one"></div>
      <div className="circle-two"></div>
      <div className="circle-three"></div>

      {/* Sign-in content */}
      <SignIn />
    </div>
  );
}

export default App;
