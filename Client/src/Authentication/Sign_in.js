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
      const message = await response.text(); // You can change this to JSON if your API returns JSON
      console.log('Login successful:', message);
      setError('');
      // redirect, store user info, etc
    } else {
      const errorText = await response.text();
      setError(errorText || 'Login failed');
    }
  } catch (error) {
    console.error('Error during login:', error);
    setError('Network error');
  }
};
