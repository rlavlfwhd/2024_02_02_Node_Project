const express = require('express');
const fs = require('fs');
const router = express.Router();

const initalResources = {
    metal : 500,
    crystal : 300,
    deuterium : 100,
}

global.players = {};

router.post('/login' , (req, res) => {

    const {name , password} = req.body;

    if(!global.players[name]){
        return res.status(404).send({message: '플레이어를 찾을 수 없습니다.'});
    }

    if(password !== global.players[name].password)
    {
        return res.status(401).send({message: '비밀번호가 틀렸습니다.'});
    }

    const player = global.players[name];
    
    const responsePayload = {
        playerName: player.playerName,
        metal: player.resources.metal,
        crystal: player.resource.crystal,
        deuterium: player.resource.deuterium
    }

    console.log("Login response payload" , responsePayload);
    res.send(responsePayload);
});


module.exports = router;

router.post('/register', (req, res) => {
    const {name , password} = req.body;

    if(global.players[name])
    {
        return res.status(400).send({ message : '이미 등록된 사용자입니다.'});
    }

    global.players[name] = {

        playerName: name,
        password: password,
        resources:{
            metal : 500,
            crystal : 300,
            deuterium : 100
        },
        planets:[]
    };

    router.post('/collect/:playerName' , (req, res) => {
        const player = global.players[req.params.playerName];
        
        if(!player)
        {
            return res.status(404).send({message:'플레이어를 찾을 수 없습니다. '});
        }

        player.resources.metal += 10;
        player.resources.crystal += 5;
        player.resources.deuterium += 2;

        console.log(player.resources);
        
        saveResources();
        res.send(player.resources);
    });

    saveResources();
    res.send({message : '등록 완료' , Player:name});
});

function saveResources() {

    fs.writeFileSync('resources.json'. JSON.stringify(global.players, null, 2));
}