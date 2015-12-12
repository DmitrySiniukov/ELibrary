var headerElement;

function init() {
	window.onscroll = function () { onScrolling(); };
	headerElement = document.getElementById("header");
	headerElement.onmouseover = function () {
	    headerElement.style.opacity = "1";
	}
	headerElement.onmouseout = function () {
	    onScrolling();
	}

	var authors = document.getElementsByClassName('author-p');
	var titles = document.getElementsByClassName('title-h');
	for (var i = 0; i < authors.length; i++) {
		setWidth(authors[i]);
	}
	for (var i = 0; i < titles.length; i++) {
		setWidth(titles[i]);
	}
}

function onScrolling() {
	if (document.body.scrollTop || document.documentElement.scrollTop > 50) {	
		headerElement.style.opacity = "0.6";
	} else {
	    headerElement.style.opacity = "1";
	}
}

function setWidth(elem) {
	elem.style.width = $(elem).parents("div[class='book-block']").find("div[class='book-image']").width().toString() + 'px';
}

function publishChecked(publish) {
	var url = "/Admin/PublishRange?mode=" + (publish ? "publish" : "unpublish") + "&ids=" + getCheckedIds();
	window.location.assign(url);
}

function removeChecked() {
	var url = "/Admin/RemoveRange?ids=" + getCheckedIds();
	window.location.assign(url);
}

function getCheckedIds() {
	var checkboxes = $("input[type=checkbox][name='multi-choice']:checked");
	var ids = "";
	if (checkboxes.length > 0) {
		var ids = $(checkboxes[0]).parents('tr').find(".order-num").text();
		for (var i = 1; i < checkboxes.length; i++) {
			ids = ids + ',' + $(checkboxes[i]).parents('tr').find(".order-num").text();
		}
	}
	return ids;
}