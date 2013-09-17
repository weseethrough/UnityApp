#pragma strict

function Start () {

}



function Update () {
 if (transform.position.z > -2000)
 {
 transform.position.z= transform.position.z -3;
 }
 else{
 transform.position.z = 2000;
 }
 

}