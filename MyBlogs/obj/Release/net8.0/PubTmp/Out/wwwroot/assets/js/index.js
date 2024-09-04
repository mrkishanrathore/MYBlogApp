import { BlogsToShow, setBlogsToShow } from './blogdata.js';
const EndPoint = "api";
$(document).ready(function () {
    let currentBlogId;

    // Hide initial elements
    $("#loginFrom, #signupFrom, .msglogout").hide();
    checkIfLogin();
    showAllBlogs();

    // Add Comment
    $("#addComment").click(function (e) {
        e.preventDefault();
        let comment = $("#inputComment").val();
        let user = localStorage.getItem("user");
    
        if (comment !== "") {
            if (user) {
                user = JSON.parse(user);
                let data = {
                    "userId": user.userId,
                    "blogId": currentBlogId,
                    "commentText": comment,
                };
    
                $.ajax({
                    type: "POST",
                    url: `${EndPoint}/Blogs/addComment`,
                    data: JSON.stringify(data),
                    contentType: "application/json",
                    success: function () {
                        $("#inputComment").val("");
                        let updateBlogs = BlogsToShow;
                        updateBlogs.find(blog => blog.blogId == currentBlogId).comments.push({...data, userName: user.userName});
                        setBlogsToShow(updateBlogs);
                        addCommentToDisplay({...data, userName: user.userName});
                        let $commentsArea = $('.commentsArea');
                        $commentsArea.scrollTop($commentsArea.prop('scrollHeight'));
                    },
                    error: function (xhr) {
                        handleAjaxError(xhr);
                    }
                });
            } else {
                alert("Login to Add Comments");
            }
        }
    });
    
    // Handle Show Comments
    function handleComment(blogId) {
        let commentArea = $(".commentsArea") ;
        if (currentBlogId === blogId && commentArea.hasClass("show")) {
            commentArea.animate({ right: '-32vw' }).removeClass("show");
        }else{
            commentArea.animate({ right: '0vw' }).addClass("show");
        }
        currentBlogId = blogId;
        let comments = BlogsToShow.find(blog => blog.blogId == blogId).comments;
        displayComments(comments);
        scrollCommentsAreaTo("bottom");
    }

    // Search Handle
    $("#serachInput").keyup(function () {
        let key = $("#serachInput").val().trim().toLowerCase();

        if (key && BlogsToShow.length) {
            let filteredBlogs = BlogsToShow.filter(blog => blog.title.trim().toLowerCase().includes(key));
            addBlogs(filteredBlogs);
        }else{
            showAllBlogs()
        }
    });

    // Open Login Form
    $("#login").click(function () {
        toggleBlur(true);
        scrollToTop();
        $("#loginFrom").show();
    });

    // Open Signup Form
    $("#signup").click(function (e) {
        e.preventDefault();
        scrollToTop();
        toggleBlur(true);
        $("#signupFrom").show();
    });

    // Handle Logout
    $("#logout").click(function () {
        localStorage.removeItem("user");
        location.reload();
    });

    // Handle Home Click
    $("#home").click(function (e) {
        e.preventDefault();
        location.reload();
    });

    // Handle MyBlogs Click
    $("#MyBlogs").click(function (e) {
        e.preventDefault();
        let user = JSON.parse(localStorage.getItem("user"));
        if (user) {
            showBlogsWithId(user.userId);
            $(".active").removeClass("active");
            $(this).addClass("active");
        }
    });

    // Handle restart server
    $("#Restart").click(function (e) {
        console.log("Restart");
        $.ajax({
            type: "POST",
            url: `${EndPoint}/Restart`,
            data: JSON.stringify(""),
            dataType: "json",
            contentType: "application/json",
            success: function (response) {
                console.log(response);
            },
            error: function (xhr) {
                handleAjaxError(xhr);
            }
        });

            
    });

    // Handle Login Submit
    $("#submitLogin").click(function (e) {
        e.preventDefault();
        let email = $("#loginEmailInput").val();
        let password = $("#loginPasswordInput").val();

        if (email && password) {
            $.ajax({
                type: "POST",
                url: `${EndPoint}/Users/checkuser`,
                data: JSON.stringify({ email, password }),
                dataType: "json",
                contentType: "application/json",
                success: function (response) {
                    storeUser(response);
                    checkIfLogin();
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                }
            });

            toggleBlur(false);
            $("#loginFrom").hide();
        }
    });

    // Handle Signup Submit
    $("#submitSignUp").click(function (e) {
        e.preventDefault();
        let userName = $("#signupNameInput").val();
        let email = $("#signupEmailInput").val();
        let password = $("#signupPasswordInput").val();

        if (userName && email && password) {
            $.ajax({
                type: "POST",
                url: `api/Users/adduser`,
                data: JSON.stringify({UserName:userName,Email : email,Password: password }),
                dataType: "json",
                contentType: "application/json",
                success: function (response) {
                    storeUser(response);
                    checkIfLogin();
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                }
            });

            toggleBlur(false);
            $("#signupFrom").hide();
        }
    });

    // Close login and signup forms if clicking outside
    $(document).click(function (e) {
        if (!$(e.target).closest('#loginFrom, #signupFrom, #login, #signup, .commentBtn').length) {
            if ($("#loginFrom").is(":visible")) {
                $("#loginFrom").hide();
            }
            if ($("#signupFrom").is(":visible")) {
                $("#signupFrom").hide();
            }
            let commentArea = $(".commentsArea");
            if (commentArea.hasClass("show")) {
                commentArea.animate({ right: '-32vw' }).removeClass("show");
            }
            toggleBlur(false);
        }
    });

    // Prevent click events inside login and signup forms from closing them
    $('#loginFrom, #signupFrom, .commentsArea').click(function (e) {
        e.stopPropagation();
    });

    // Toggle blur effect
    function toggleBlur(enable) {
        if (enable) {
            $(".blogBody").addClass("blur");
            $("body").addClass("onScroll");
        } else {
            $(".blogBody").removeClass("blur");
            $("body").removeClass("onScroll");
        }
    }

    // Show and hide elements based on login status
    function enableElements() {
        $(".myBlogLink, .dropDownLink").removeClass("disabled");
        $(".msglogout").show();
        $(".loginSigninBtns").hide();
        $("#inputComment").prop("disabled", false).prop("placeholder","Add Comment...");
    }

    function disableElements() {
        $(".myBlogLink, .dropDownLink").addClass("disabled");
        $(".msglogout").hide();
        $(".loginSigninBtns").show();
        $("#inputComment").prop("disabled", true).prop("placeholder", "Login To Comment...");

    }

    // Store user data in local storage
    function storeUser(user) {
        localStorage.setItem("user", JSON.stringify(user));
    }

    // Check if user is logged in
    function checkIfLogin() {
        let user = JSON.parse(localStorage.getItem("user"));
        if (user) {
            enableElements();
            $("#showName").text(user.userName);
        } else {
            disableElements();
        }
    }

    // Fetch and show all blogs
    function showAllBlogs() {
        $.ajax({
            type: "GET",
            url: `${EndPoint}/blogs`,
            dataType: "json",
            success: function (response) {
                setBlogsToShow(response);
                addBlogs(response);
            },
            error: function (xhr) {
                handleAjaxError(xhr);
            }
        });
    }

    // Fetch and show blogs for a specific user ID
    function showBlogsWithId(id) {
        $.ajax({
            type: "GET",
            url: `${EndPoint}/blogs/${id}`,
            dataType: "json",
            success: function (response) {
                addBlogs(response);
            },
            error: function (xhr) {
                handleAjaxError(xhr);
            }
        });
    }

    // Add blogs to the page
    function addBlogs(blogs) {
        let blogHtmlList = blogs.map(blog => {
            let imageSrc = '';

            if (blog.imageData && blog.imageData.length > 0) {
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
            <button type="button" data-blog-id="${blog.blogId}" class="commentBtn ml-auto btn btn-primary position-relative">
                Comments <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill text-bg-secondary">${blog.comments.length}</span>
            </button>
        </div>
    `;
        }).join('');

        $("#blogDisplayArea").html(blogHtmlList);

        $(".commentBtn").click(function () {
            const blogId = $(this).data('blog-id');
            handleComment(blogId);
        });
    }


    // Display comments for a blog
    function displayComments(comments) {
        let commentsHtml = comments.map(comment => `
            <div class="comment border rounded-3 mt-2 p-2">
                <div class="fs-5 commentUserName text-light">User : <span class="text-success">${comment.userName}</span></div>
                <div class="fs-5 commentDescription">${comment.commentText}</div>
            </div>
        `).join('');

        $("#addComments").html(commentsHtml);
    }

    function addCommentToDisplay(comment){
        let allComments =  $("#addComments");
        allComments.html(allComments.html()+`<div class="comment border rounded-3 mt-2 p-2">
                <div class="fs-5 commentUserName text-light">User : <span class="text-success">${comment.userName}</span></div>
                <div class="fs-5 commentDescription">${comment.commentText}</div>
            </div>`);
        scrollCommentsAreaTo("bottom");
    }

    // Handle AJAX errors
    function handleAjaxError(xhr) {
        if (xhr.status === 400) {
            console.error('Error:', xhr.responseText);
            alert(xhr.responseText);
        } else {
            console.error('Error:', xhr.statusText);
            alert('An unexpected error occurred.');
        }
    }

    function scrollToTop() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function scrollCommentsAreaTo(position) {
        let $element = $(".commentsArea");
    
        if (position === "bottom") {
            $element.scrollTop($element.prop('scrollHeight'));
        } else if (position === "top") {
            $element.scrollTop(0);
        } else {
            console.error('Invalid position. Use "top" or "bottom".');
        }
    }
});
