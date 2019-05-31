# TFG-Vuforia
## Para generar la aplicación en Unity
1. Abrimos en Unity el proyecto.
2. A continuación debemos seleccionar la escena en la que se encuentra la aplicación en
Unity. Esta escena se encuentra en el directorio principal *Assets>Scenes* y su nombre
es **CloudRecognition**.
3. Una vez localizada la escena principal, debemos seleccionarla. Para ello la arrastraremos
al panel de trabajo (situado arriba a la izquierda).
4. El siguiente paso es exportar este proyecto en Unity para que pueda usarlo la parte
de Android. Para ello, pulsaremos *File>Build Settings*. Es muy importante antes de exportar
la aplicación a Android tener seleccionado Android. Para seleccionarlo se marca y se presiona
el botón de Switch Platform.
Una vez que nos hemos asegurado de tener seleccionada la plataforma Android ya
podemos exportar el proyecto. Para ello seleccionamos la opción que dice **Export
proyect** con el Build system de Gradle.

## Cómo unir la parte de Unity y Android
Como solución de la sección anterior se habrá generado en la carpeta especificada un
proyecto en Android llamado **FilmAR**.
Si fuésemos a crear un proyecto en Android desde cero, partiríamos desde este proyecto
generado por Unity.
Si lo que queremos es actualizar nuevos cambios en Unity en la aplicación de Android
seguiremos los siguientes pasos:
1. Accederemos a los directorios *FilmAR>src>main* de las dos aplicaciones (la generada
por Unity con los nuevos cambios y la aplicación en Android general de la aplicación).
2. Eliminaremos la carpeta Assets de la aplicación general y la cambiaremos por la nueva
carpeta Assets generada por Unity.

De esta manera, tendremos la aplicación en Android actualizada a los nuevos cambios de la
parte de Realidad Aumentada.
