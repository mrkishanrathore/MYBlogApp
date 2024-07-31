const EndPoint = "api";
let myBlogs = [];
let currentBlogId = "";
$(document).ready(function () {
    let user = JSON.parse(localStorage.getItem("user"));
    showBlogsWithId(user.userId);
    $("#updateFrom").hide();

    $("#updateBlogForm").on("submit", function (e) {
        e.preventDefault();
        
        let blogTitle = $("#blogTitle").val();
        let blogDescription = $("#blogDescription").val();
        let blogImageFile = $("#blogImage")[0].files[0];
        
        if(blogTitle != "" && blogDescription != "" && blogImage != ""){
            imageToByteArray(blogImageFile, function (byteArray) {
                handleUpdateBlogSubmission(blogTitle, blogDescription, byteArray);
            });
        }else{
            showMsg("warning","All Fields are required.");
        }
        
    });

    $('#updateFrom').click(function (e) {
        e.stopPropagation();
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

    $(document).click(function (e) {
        if (!$(e.target).closest('#updateFrom, .updateBtn').length) {
            if ($("#updateFrom").is(":visible")) {
                $("#updateFrom").hide();
            }
            toggleBlur(false);
        }
    });
});

function handleUpdateBlogSubmission(title, description, image) {
    let user = JSON.parse(localStorage.getItem("user"));
    let data ={
        "userId": user.userId,
        "blogId": currentBlogId,
        "image": "https://picsum.photos/500/500",
        "imageData": image,
        "title": title,
        "description": description
    }
    $.ajax({
        type: "PUT",
        url: `${EndPoint}/Blogs`,
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json",
        success: function (response) {
            /*showMsg("success", "Updated Blog SuccessFully.");
            setTimeout(() => { location.reload(); }, 2000);*/
            location.reload();
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

    // Toggle blur effect
function toggleBlur(enable) {
    if (enable) {
        $(".mainBody").addClass("blur");
        // $("body").addClass("onScroll");
    } else {
        $(".mainBody").removeClass("blur");
        $("body").removeClass("onScroll");
    }
}

function showBlogsWithId(id) {
    $.ajax({
        type: "GET",
        url: `${EndPoint}/blogs/${id}`,
        dataType: "json",
        success: function (response) {
            addBlogs(response);
            myBlogs = response;
        },
        error: function (xhr) {
            showMsg('warning',"No Blogs To Show.")
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

function byteArrayToImage(byteArray) {
    const blob = new Blob([byteArray], { type: 'image/png' }); // Adjust the MIME type as needed
    return URL.createObjectURL(blob);
}

function addBlogs(blogs) {
    let blogHtmlList = blogs.map(blog => {
        let imageSrc = '';

        if (blog.imageData && blog.imageData.length > 0) {
            console.log(blog.imageData);
            // Convert the imageData byte array to an image URL
            imageSrc = `data:image/png;base64,${blog.imageData}`;
        } else {
            // Provide a placeholder or empty image if no image data
            imageSrc = "https://picsum.photos/500/500";
        }

        return `
        <div class="blog my-3 p-2 rounded shadow">
            <h2 class="text-center my-2">${blog.title}</h2>
            <div class="imgContainer m-2">
                <img class="imageSize" src="${imageSrc}" alt="Blog Image">
            </div>
            <div class="blogDesc fs-5 m-4">${blog.description}</div>
            <div class="blogDesc bolder fs-4 mb-3">BY ${blog.userName}</div>
            <button type="button" data-blog-id="${blog.blogId}" class="updateBtn ml-auto mx-2 btn btn-primary position-relative">
                Update
            </button>
            <button type="button" data-blog-id="${blog.blogId}" class="deleteBtn ml-auto mx-2 btn btn-danger position-relative">
                Delete
            </button>
        </div>
    `;
    }).join('');

    $("#blogDisplayArea").html(blogHtmlList);

    $(".updateBtn").click(function () {
        const blogId = $(this).data('blog-id');
        handleUpdate(blogId);
    });

    $(".deleteBtn").click(function () {
        const blogId = $(this).data('blog-id');
        if (confirm("Are you sure you want to delete this item?")) {
            handleDelete(blogId);
        }
    });
}


function handleDelete(blogId) {

    $.ajax({
        type: 'DELETE',
        url: `${EndPoint}/Blogs`,
        data: JSON.stringify({
            'blogId': blogId,
            'userId': JSON.parse(localStorage.getItem('user')).userId,
        }),
        dataType: "json",
        contentType: "application/json",
        success: function (data, textStatus, xhr) {
            scrollToTop();
            location.reload();
        },
        error: function (xhr, textStatus, errorThrown) {
            showMsg("danger", "Unable To DELETE .");
        }
    });
}

function handleUpdate(blogId){
    // Setting Values to From
    let blog = myBlogs.find(blog => blog.blogId === blogId);
    $("#blogTitle").val(blog.title);
    $("#blogDescription").val(blog.description);
    // $("#blogImage").val();
    currentBlogId = blogId;
    $("#updateFrom").show();
    scrollToTop();
    toggleBlur(true);
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