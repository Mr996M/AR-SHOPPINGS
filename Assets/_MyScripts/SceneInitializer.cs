using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Linq;


/**
 * The script that will run at the very begining to init all the necessary info on the server 
 * In reality the server will hold all those info from the blue print of the building
 * However in this demo, data are stored in unity locally, so we have to send them to the server first
 */
public class SceneInitializer : MonoBehaviour
{
    [HideInInspector]
    private String serverIP = "127.0.0.1";
    [HideInInspector]
    private int serverPort = 8081;


    // Start is called before the first frame update
    void Start()
    {
        // connect to the server
        var socket = Utils.connectToServer(this.serverIP, this.serverPort, 1);
        if (socket == null) {
            Debug.Log("fail to connect to server for initialization, skipping this part...");
            return;
        }

        // init all the infomation of the sensors 
        if (!initSensors(socket)) {
            Debug.Log("fail to init sensor data in the server!");
            socket.Close();
            Utils.quit();
            return;
        }

        // init all map of all the land marks 
        if (!initLandMarkMap(socket)) {
            Debug.Log("fail to init land mark map in the server!");
            socket.Close();
            Utils.quit();
            return;
        }

        // init the information of all the land marks
        if (!initLandMarkInfo(socket)) {
            Debug.Log("fail to init the land mark info in the server!");
            socket.Close();
            Utils.quit();
            return;
        }

        socket.Close();
    }



    // Update is called once per frame
    void Update()
    {
        
    }



    private bool initSensors(Socket socket) {

        var sensorList = GameObject.FindGameObjectsWithTag("sensor");
        var timeOut = 5;

        var message_byte = Encoding.UTF8.GetBytes(buildSensorsMessage(sensorList));
        socket.Send(message_byte, message_byte.Length, 0);

        return Utils.acknowledge(socket);

    }



    private String buildSensorsMessage(GameObject[] sensorList) {

        /* example: 
           {
                "Sensor_LivingRoom_1": {"x": 1.1, "y": 2.2, "height": 3.3, "floor": 1}, 
                "Sensor_LivingRoom_2": {"x": 1.2, "y": 2.3, "height": 3.4, "floor": 1}
            }
         */

        var message = "{";

        for (int i = 0; i < sensorList.Length; i++) {
            message += ("\"" + sensorList[i].name + "\": {");
            message += ("\"x\": " + sensorList[i].transform.position.x + ", ");
            message += ("\"y\": " + sensorList[i].transform.position.z + ", ");
            message += ("\"height\": " + sensorList[i].transform.position.y + ", ");
            message += ("\"floor\": " + sensorList[i].GetComponent<Sensor>().floorNum + "}");
            if (i != sensorList.Length - 1) { message += ", "; }
        }

        message += "}";

        return message;

    }



    private bool initLandMarkMap(Socket socket) {

        var message_byte = Encoding.UTF8.GetBytes(buildMap());
        var timeOut = 5;

        socket.Send(message_byte, message_byte.Length, 0);

        return Utils.acknowledge(socket);

    }



    private String buildMap() {

        var marks = GameObject.FindGameObjectsWithTag("mark");

        var marks_dict = new Dictionary<String, GameObject>();
        foreach (GameObject mark in marks) {
            marks_dict.Add(mark.name, mark);
        }

        return @"
        {""node_cathy1"": {""node_cathy2"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["node_cathy2"]) + @", ""node_cathy3"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["node_cathy3"]) + @", ""node_cathy4"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["node_cathy4"]) + @", ""node_cathy5"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["node_cathy5"]) + @", ""Toilets_1"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["Toilets_1"]) + @", ""node_bill3"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["node_bill3"]) + @", ""Toilets_2"": " + Utils.directDistance(marks_dict["node_cathy1"], marks_dict["Toilets_2"]) + @"},
        ""entrance"": {""node_cathy1"": " + Utils.directDistance(marks_dict["entrance"], marks_dict["node_cathy1"]) + @"}}
        ";

    }



    private bool initLandMarkInfo(Socket socket) {

        var marks = GameObject.FindGameObjectsWithTag("mark");
        var timeOut = 5;

        var message = "{";
        for (int i = 0; i < marks.Length; i++) {
            var markPosition = marks[i].transform.position;
            message += ("\"" + marks[i].name + "\"" + ": {\"x\": " + markPosition.x + ", \"y\": " + markPosition.z + ", \"height\": " + markPosition.y + "}");
            if (i != marks.Length - 1) { message += ","; }
        }
        message += "}";

        var message_byte = Encoding.UTF8.GetBytes(message);
        socket.Send(message_byte, message_byte.Length, 0);

        return Utils.acknowledge(socket);

    }
    
}
