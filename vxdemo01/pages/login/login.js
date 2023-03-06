Page({
  handlSignup(){
    wx.getUserProfile({
      desc: '用于注册',
    }).then(userinfo=>{
      wx.login({
        success: (logininfo) => {
          wx.request({
            url: 'https://localhost:7148/Home/SignUp?code='+logininfo.code,
            method:'POST',
            data:userinfo.userInfo,
            success:(res)=>{
              wx.showToast({
                title: '注册成功',
                icon: 'success',
                duration: 2000
                })
            }
          })
        },
      })
    }).catch(err=>{
      console.log(err)
    })
  },

  handleLogin(){
    
    wx.login({
      success: (res) => {
        wx.request({
          url: 'https://localhost:7148/Home/Login?code='+res.code,
          success(res){
            console.log(res)
            wx.setStorage({
              key:"login-key",
              data:'Bearer '+res.data
            })
            wx.redirectTo({
              url: '/pages/index/index',
              success: (res) => {
                console.log('@',res);
              },
              fail: (res) => {
                console.log('#',res);
              },
              complete: (res) => {},
            })
          }
        })
      },
    })
  }
})