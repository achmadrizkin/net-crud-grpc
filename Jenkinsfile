def Tag_Release() {
    sh 'git describe --tags --abbrev=0 > Tag_Release'
    def Tag_Release = readFile('Tag_Release').trim()
    sh 'rm Tag_Release'
    Tag_Release
}

pipeline {
    agent {
        node {
            label 'slave-00'
            customWorkspace "workspace/${env.BRANCH_NAME}/src/git.bluebird.id/bb-one/net_test_generator_svc"
        }
    }
    environment {
        SERVICE = 'net_test_generator_svc-bbone'
    }
    options {
        buildDiscarder(logRotator(daysToKeepStr: env.BRANCH_NAME == 'master' ? '90' : '30'))
    }
    stages {
        stage('Checkout') {
            when {
                anyOf { branch 'master'; branch 'develop'; branch 'staging' }
            }
            steps {
                echo 'Checking out from Git'
                checkout scm
            }
        }

        stage('Prepare') {
            steps {
                withCredentials([file(credentialsId: '3521ab7f-3916-4e56-a41e-c0dedd2e98e9', variable: 'sa')]) {
                sh "cp $sa service-account.json"
                sh "chmod 644 service-account.json"
                sh "docker login -u _json_key --password-stdin https://asia.gcr.io < service-account.json"
                }
            }
        }

        stage('Build and Deploy') {
            environment {
                VERSION_PREFIX = '1.0'
            }
            stages {
                stage('Deploy to development') {
                    when {
                        branch 'develop'
                    }
                    environment {
                        ALPHA = "${env.VERSION_PREFIX}-alpha${env.BUILD_NUMBER}"
                        NAMESPACE="helical-element-182307"
                    }
                    steps {
                        withCredentials([file(credentialsId: 'b5da36c4-6c1c-495e-b17f-839796f54f0a', variable: 'kubeconfig')]) {
                        sh "cp $kubeconfig kubeconfig.conf"
                        sh "chmod 644 kubeconfig.conf"
                        sh "gcloud auth activate-service-account --key-file service-account.json"
                        sh 'chmod +x ./build.sh'
                        sh './build.sh $ALPHA'
                        sh 'chmod +x ./deploy.sh'
                        sh './deploy.sh $ALPHA $NAMESPACE bb-one'
                        sh 'rm kubeconfig.conf service-account.json'
                        }
                    }
                }
                stage('Deploy to production') {
                    when {
                        branch 'master'
                    }
                    environment {
                        NAMESPACE="bbone-prod-1902"
                        VERSION = VersionNumber([
                            versionNumberString: '${BUILDS_ALL_TIME}',
                            worstResultForIncrement: 'SUCCESS',
                            versionPrefix : '1.0.'
                        ]);
                    }
                    steps {
                        withCredentials([file(credentialsId: 'c6cb8a83-f863-4b17-80be-65908b57b47d', variable: 'kubeconfig')]) {
                            sh "cp $kubeconfig kubeconfig.conf"
                            sh "chmod 644 kubeconfig.conf"
                            sh "gcloud auth activate-service-account --key-file service-account.json"   
                            sh 'chmod +x build.sh'
                            sh './build.sh $VERSION'
                            sh 'chmod +x deploy.sh'
                            sh './deploy.sh $VERSION $NAMESPACE bb-one'
                            sh 'rm kubeconfig.conf service-account.json'
                        }
                    }
                }
            }
        }
    }
}
