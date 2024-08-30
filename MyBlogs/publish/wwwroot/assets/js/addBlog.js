import { BlogsToShow, setBlogsToShow } from './blogdata.js';
const EndPoint = "../../api";

$(document).ready(function () {

    checkUserAndNavigate();
    $("#addBlogForm").on("submit", function (e) {
        e.preventDefault();
        
        let blogTitle = $("#blogTitle").val();
        let blogDescription = $("#blogDescription").val();
        let blogImage = $("#blogImage")[0].files[0]; 
        
        if (blogTitle && blogDescription && blogImage){
            imageToByteArray(blogImage, function (byteArray) {
                handleBlogSubmission(blogTitle, blogDescription, byteArray);
            });
        }else{
            showMsg("warning","All Fields are required.");
        }
        
    });

    $("#blogImage").on("change", function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $("#imagePreview").attr("src", e.target.result).show();
            }
            reader.readAsDataURL(file);
        } else {
            $("#imagePreview").hide();
        }
    });

});

function checkUserAndNavigate() {
    // Check if the user is present in localStorage
    if (!localStorage.getItem('user')) {
        // If not present, navigate to index.html
        window.location.href = '../../index.html';
    }
}

function handleBlogSubmission(title, description, image) {
    let user = JSON.parse(localStorage.getItem("user"));
    let data ={
        "userId": user.userId,
        "image": "https://picsum.photos/500/500",
        "imageData": image,
        "title": title,
        "description": description
    }
    console.log(image);
    $.ajax({
        type: "POST",
        url: `${EndPoint}/Blogs/addBlog`,
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json",
        success: function (response) {
            addToLocal(response);
            showMsg("success", "Added Blog SuccessFully.");
            $("#addBlogForm")[0].reset();
            $("#imagePreview").hide();
        },
        error: function (xhr) {
            if (xhr.status === 400) {
                showMsg('danger', xhr.responseText);
            } else {
                showMsg('danger',`${xhr.statusText} An unexpected error occurred.`);
            }
        }
    });
    
}

function showMsg(type, msg){
    $("#msg").html(`<div class="alert alert-${type} alert-dismissible fade show" role="alert">
        <strong>${type=="danger"?"Error":type.toUpperCase()}! </strong> ${msg}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>`
    );
    scrollToTop();
}
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}
function addToLocal(blog){
    let updateBlogs = BlogsToShow;
    updateBlogs.unshift(blog);
    setBlogsToShow(updateBlogs);
}

function arrayBufferToBase64(arrayBuffer) {
    let binary = '';
    const bytes = new Uint8Array(arrayBuffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

function imageToByteArray(file, callback) {
    const reader = new FileReader();

    reader.onload = function (event) {
        const arrayBuffer = event.target.result;
        const base64String = arrayBufferToBase64(arrayBuffer);
        callback(base64String);
    };

    reader.readAsArrayBuffer(file);
}