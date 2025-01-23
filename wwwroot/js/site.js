document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
        e.preventDefault();
        const login = form.querySelector('[name="UserLogin"]').value;
        const password = form.querySelector('[name="Password"]').value;
        if (login.length == 0) {
            alert("Введіть логін");
            return;
        }
        if (password.length == 0) {
            alert("Введіть пароль");
            return;
        }
        // RFC 7617
        const credentials = btoa(login + ':' + password);
        fetch("/User/Authenticate", {
            method: "GET",
            headers: {
                'Authorization': 'Basic ' + credentials
            }
        }).then(r => {
            console.log(r);
            if (r.ok) {
                window.location.reload();
            }
            else {
                r.json().then(j => {
                    alert(j);   // Д.З.
                });
            }
        });

        console.log(credentials);
    }
});
/*
Д.З. Реалізувати виведення помилок процесу автентифікації 
 у складі самої форми (модального вікна).
 В авторизованому режимі відокремити першу літеру імені
 користувача, перевести її у верхній реєстр та зобразити у 
 кольоровому колі.
 У підказці, що спливає при наведенні, вивести повне ім'я
*/