document.getElementById('login-form').addEventListener('submit', function (event) {
    event.preventDefault();

    // Get input values
    var username = document.getElementById('username').value;
    var password = document.getElementById('password').value;

    // Hardcoded credentials
    var validUsername = 'admin';
    var validPassword = 'password';

    // Simple authentication check
    if (username === validUsername && password === validPassword) {
        // Save login state in sessionStorage
        sessionStorage.setItem('loggedIn', 'true');

        // Redirect to dashboard
        window.location.href = 'dashboard.html';
    } else {
        // Display error message
        var messageDiv = document.getElementById('message');
        messageDiv.textContent = 'Invalid username or password.';
    }
});
