import sys
import subprocess
import os
import shutil

def main():
    # Retrieve variables from command-line arguments
    if len(sys.argv) < 7:
        print("Error: Missing required arguments.")
        sys.exit(1)

    environment = sys.argv[1]
    assembly_name = sys.argv[2]
    github = sys.argv[3]
    database = sys.argv[4] 
    project_path = sys.argv[5]
    template_path = sys.argv[6]

    # Use the variables in your logic
    #print(f"Deployment environment: {environment}")
    #print(f"Assembly name: {assembly_name}")
    #print(f"Github: {github}")
    #print(f"Database: {database}")
    #print(f"Project path: {project_path}")
    #print(f"Template path: {template_path}")

   # Run azd login
    try:
        subprocess.run(["azd", "auth", "login", "--scope", "https://management.azure.com//.default"], check=True)
        print("Successfully logged in using 'azd login'")
    except subprocess.CalledProcessError as e:
        print(f"Error running 'azd login': {e}")
        sys.exit(1)
       
    # Run azd init
    try:
        subprocess.run(["azd", "init","-e", environment, "--cwd", project_path ], check=True)
        print("Successfully ran 'azd init'")

    except subprocess.CalledProcessError as e:
        print(f"Error running 'azd init': {e}")
        sys.exit(1)

     #if using MongoDB, run azd infra synth to create the bicep file for the mongodb module
    if database == "MongoDb":
        try:
            subprocess.run(["azd", "infra", "synth"], check=True)
            print("Successfully ran 'azd infra synth'")
        except subprocess.CalledProcessError as e:
            print(f"Error running 'azd infra synth': {e}")
            sys.exit(1)

        # Copy mongodb.module.bicep to the hosting project's infra folder if it exists
        template_path = os.path.join(template_path, 'mongodb.module.bicep')
        infra_folder_path = os.path.join(f'infra', 'mongodb')

        if os.path.isfile(template_path):
            if not os.path.exists(infra_folder_path):
                os.makedirs(infra_folder_path)
            shutil.copy(template_path, infra_folder_path)
            print(f"Copied '{template_path}' to '{infra_folder_path}'")
        else:
            print(f"File '{template_path}/mongodb.module.bicep' does not exist.")
            sys.exit(1)

    if github:
        try:
            subprocess.run(["azd", "pipeline","config", "-e", environment], check=True)
            print("Successfully ran 'azd github setup for environment'")
        
        except subprocess.CalledProcessError as e:
            print(f"Error running 'azd github setup for environment '{environment}': {e}")
            sys.exit(1)        

if __name__ == "__main__":
    main()