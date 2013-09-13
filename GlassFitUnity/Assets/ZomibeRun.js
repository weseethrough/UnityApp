#pragma strict

function Start () {

}



function Update () {
 if (transform.position.z > -2900)
 {
 transform.position.z= transform.position.z -10;
 }
 else{
 transform.position.z = 2700;
 }
 

}