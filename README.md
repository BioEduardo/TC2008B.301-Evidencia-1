# TC2008B.301-Evidencia-1

*Evidencia 1. Actividad Integradora*

Integrantes:
Eduardo Joel Cortez Valente A01746664
David Damián Galán A01752785
Paulo Ogando Gulias A01751587
José Ángel García Gómez A01745865

Descripción del medio ambiente:
Los agentes involucrados en esta interacción son los robots, las cajas y los estantes. También
incluimos como agentes inmóviles a las paredes y a la puerta. La forma de interacción
inteligente está planteada de la siguiente manera: Los robots buscarán las cajas generadas
aleatoriamente, los buscarán moviéndose aleatoriamente por el grid. Una vez que hayan
encontrado una caja, esta será recogida y será llevada al estante más cercano al robot. Cada
estante podrá mantener hasta 5 cajas.
Los robots tendrán un booleano que indicará si está agarrando una caja o no. Si este es True,
se cambiará el modelo del robot en la simulación, y no se le permitirá agarrar más cajas.
Los estantes tendrán un contador de cajas, cada que un robot llegue a un estante y
deposite una caja, ésta se incrementará en 1. Al llegar a 5, el estante es removido de la lista de
estanterías que contiene el modelo, de este modo los robots ya no lo tendrán en cuenta en el
proceso de búsqueda del estante más cercano

 
