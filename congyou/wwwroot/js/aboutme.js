var count = 0;
function change(a) {
    document.getElementById("image").src = a;
    ++count;
    if (count % 3 == 1) {
        document.getElementById("text").innerHTML = "Hi, I have already graduated from Syracuse University."
    }
    else if (count % 3 == 2) {
        document.getElementById("text").innerHTML = "A weeb and the girl above is my top waifu."
    }
    else {
        document.getElementById("text").innerHTML = "Follow me on Twitter @Icyflourite"
    }
}