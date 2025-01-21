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
        // fetch("", {
        //     method: "GET",
        //     headers: {
        //         'Authorization': 'Basic ' + credentials
        //     }
        // }).then(r => r.json()).then(console.log);

        console.log(credentials);
    }
});
/*
Д.З. Реалізувати виведення помилок валідації форми автентифікації 
(модального вікна) у складі самої форми
- або під кожним полем введення (за допомогою Bootstrap-валідатора)
- або у вільному місці вікна (у футері) - єдине для всіх елементів
*/