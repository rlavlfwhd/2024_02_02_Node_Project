require('dotenv').config();
const express = requrie('express');
const bodyParser = require('body-parser');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcrypt');

const app = express();
app.use(bodyParser.json());

const users = [];
const refreshTokens = {};

const JWT_SECRET = process.env.JWT_SECRET;
const REFRESH_TOKEN_SECRET = process.env.REFRESH_TOKEN_SECRET;
const PORT = process.env.PORT || 3000;

app.post('/register' , async(req,res) => {
    const {username, password} = req.body;

    if(users.find(user => user.username === username))
    {
        return res.status(400).json({error : '이미 존재하는 사용자입니다.'});
    }

    const hashedPassword = await bcrypt.hash(password, 10);
    users.push({username , password: hashedPassword});
    console.log(hashedPassword);
    res.status(201).json({message : '회원 가입 성공'});
})

function generateAccessToken(username)
{
    return jwt.sign({username} , JWT_SECRET, {expiresIn: '15m'});
}

function authenticateToken(req,res,next) {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];

    if(token == null) return res.sendStatus(401);

    jwt.verify(token, JWT_SECRET, (err, user) => {
        if(err) return res.sendStatus(403);
        req.user = user;
        next();
    })
}

app.listen(PORT, () => console.log(`서버가 포트 ${PORT} 에서 실행 중 입니다. `));

app.post('/login' , async(req,res) => {
    const {username, password} = req.body;
    const user = users.find(user => user.username === username);

    if(!user || !(await bcrypt.compare(password, user.password)))
    {
        return res.status(400).json({error : '잘못된 사용자명 또는 비밀 번호 입니다.'});
    }

    const accessToken = generateAccessToken(username);
    console.log(accessToken);
    constrefreshToken = jwt.sign({username}, REFRESH_TOKEN_SECRET);
    
    refreshTokens[refreshToken] = username;

    res.json({accessToken , refreshToken});
})

app.post('/token' , async(req,res) => {
    const {refreshToken} = req.body;
    

    if(!refreshToken || !refreshToken[refreshToken])
    {
        return res.status(401);
    }

    jwt.verify(refreshToken, REFRESH_TOKEN_SECRET, (err, user) => {
        if(err) return res.sendStatus(403);
        const accessToken = generateAccessToken(user.username);
        res.json({accessToken});
    })    
})

app.post('logout',(req,res) => {
    const {refreshToken} = req.body;
    delete refreshToken[refreshToken];
    res.sendStatus(204);
})

app.get('/protected', authenticateToken, (req,res) => {
    res.json({message : '보호된 데이터에 접근 성공' , user:req.user});
})