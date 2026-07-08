const login_input = document.getElementById("logintext");
const password_input = document.getElementById("password");

const registration = document.getElementById("registration");
const login = document.getElementById("login");

const API_URL = 'http://REST/v1/Auth';
login.addEventListener('click', () =>
{
  const LoginRequest =
  {
    Login: login_input.value,
    Password: password_input.value,
  }
  fetch(API_URL + '/Login',
    {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: LoginRequest 
    });
});

registration.addEventListener('click', () => 
{
  const LoginRequest =
  {
    Login: login_input.value,
    Password: password_input.value,
  }
  fetch(API_URL + '/Registration',
    {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: LoginRequest
    });
});